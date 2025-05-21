import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

export const fetchDatasets = async () => {
  const response = await axios.get(`${API_URL}/datasets`);
  return response.data;
};

export const fetchDatasetStatistics = async (datasetId) => {
  const response = await axios.get(`${API_URL}/dataset/${datasetId}/statistics`);
  return response.data;
};

export const uploadDataset = async (name, file) => {
  const formData = new FormData();
  formData.append('name', name);
  formData.append('file', file);
  
  const response = await axios.post(`${API_URL}/dataset`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  
  return response.data;
};
