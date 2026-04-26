const CLOUD_NAME = 'dv31mt6pd';
const UPLOAD_PRESET = 'vibelink_avatars';

export async function uploadAvatar(file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('upload_preset', UPLOAD_PRESET);

  const res = await fetch(`https://api.cloudinary.com/v1_1/${CLOUD_NAME}/image/upload`, {
    method: 'POST',
    body: formData,
  });

  if (!res.ok) {
    throw new Error('Error al subir la imagen');
  }

  const data = await res.json();
  return data.secure_url;
}
