from flask import Flask, request, jsonify, send_file
import torch
import numpy as np
from monai.transforms import (
    Compose, LoadImaged, EnsureChannelFirstd, Spacingd, Orientationd, 
    ScaleIntensityRanged, CropForegroundd, Resized, ToTensord
)
from monai.networks.nets import UNet
from monai.networks.layers import Norm
from monai.inferers import sliding_window_inference
from monai.utils import set_determinism, first
from monai.data import DataLoader, Dataset
import nibabel as nib
import os
from flask_cors import CORS
import matplotlib.pyplot as plt
import base64
from io import BytesIO

app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 50 * 1024 * 1024
CORS(app)

UPLOAD_FOLDER = os.path.join(os.getcwd(), "uploads")
STATIC_FOLDER = os.path.join(os.getcwd(), "static")
os.makedirs(UPLOAD_FOLDER, exist_ok=True)
os.makedirs(STATIC_FOLDER, exist_ok=True)

device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")

# Modeli yükle
model = UNet(
    spatial_dims=3,
    in_channels=1,
    out_channels=2,
    channels=(16, 32, 64, 128, 256), 
    strides=(2, 2, 2, 2),
    num_res_units=2,
    norm=Norm.BATCH,
).to(device)

model.load_state_dict(torch.load("best_metric_model.pth", map_location=device))
model.eval()

output_mask = None  # Global değişken, slice endpointi için kullanacağız.

def prepare_single(file_path, pixdim=(1.5, 1.5, 1.0), a_min=-200, a_max=200, spatial_size=[128, 128, 64]):
    set_determinism(seed=0)
    files = [{"vol": file_path}]
    transforms = Compose([
        LoadImaged(keys=["vol"]),
        EnsureChannelFirstd(keys=["vol"]),
        Spacingd(keys=["vol"], pixdim=pixdim, mode="bilinear"),
        Orientationd(keys=["vol"], axcodes="RAS"),
        ScaleIntensityRanged(keys=["vol"], a_min=a_min, a_max=a_max, b_min=0.0, b_max=1.0, clip=True),
        CropForegroundd(keys=["vol"], source_key="vol"),
        Resized(keys=["vol"], spatial_size=spatial_size),
        ToTensord(keys=["vol"]),
    ])
    ds = first(DataLoader(Dataset(data=files, transform=transforms), batch_size=1))
    return ds["vol"]

def create_visualization(output_mask):
    plt.figure(figsize=(10, 10))
    plt.imshow(output_mask[:, :, output_mask.shape[2] // 2], cmap="jet", alpha=0.7)
    plt.title("Tumor Visualization")
    plt.axis("off")

    buf = BytesIO()
    plt.savefig(buf, format="png")
    buf.seek(0)
    plt.close()

    base64_image = base64.b64encode(buf.getvalue()).decode("utf-8")
    buf.close()

    return base64_image

@app.route('/predict', methods=['POST'])
def predict():
    global output_mask  # Slice endpointi için maskeyi saklıyoruz.
    try:
        if 'image' not in request.files:
            return jsonify({"error": "No image file provided"}), 400

        file = request.files['image']
        nii_file_path = os.path.join(UPLOAD_FOLDER, "uploaded_image.nii")
        file.save(nii_file_path)

        input_volume = prepare_single(nii_file_path).to(device)

        with torch.no_grad():
            roi_size = (128, 128, 64)
            sw_batch_size = 4
            outputs = sliding_window_inference(input_volume, roi_size, sw_batch_size, model)
            outputs = (outputs > 0.53).float()

        output_mask = outputs[0, 1].cpu().numpy()

        nii_output_path = os.path.join(STATIC_FOLDER, "tumor_segmentation.nii")
        nifti_image = nib.Nifti1Image(output_mask.astype(np.float32), affine=np.eye(4))
        nib.save(nifti_image, nii_output_path)

        # PNG visualization
        base64_visualization = create_visualization(output_mask)

        return jsonify({
            "result": "Tumor detected." if np.any(output_mask) else "No tumor detected.",
            "nii_path": f"static/tumor_segmentation.nii",
            "visualization": base64_visualization
        })

    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route('/slice', methods=['GET'])
def get_slice():
    global output_mask
    try:
        slice_index = int(request.args.get("index", 0))
        if output_mask is None:
            return jsonify({"error": "No prediction has been made yet."}), 400

        if slice_index < 0 or slice_index >= output_mask.shape[2]:
            return jsonify({"error": "Slice index out of range."}), 400

        slice_image = output_mask[:, :, slice_index]

        # Slice görselleştirme
        buf = BytesIO()
        plt.imshow(slice_image, cmap="jet", alpha=0.7)
        plt.axis("off")
        plt.savefig(buf, format="png")
        buf.seek(0)
        base64_image = base64.b64encode(buf.getvalue()).decode("utf-8")
        buf.close()

        return base64_image
    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route('/static/<path:filename>')
def serve_static(filename):
    return send_file(os.path.join(STATIC_FOLDER, filename))

if __name__ == '__main__':
    app.run(debug=True)

