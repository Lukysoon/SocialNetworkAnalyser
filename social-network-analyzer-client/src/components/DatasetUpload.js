import React, { useState } from 'react';
import { uploadDataset } from '../services/datasetService';

const DatasetUpload = ({ onSuccess }) => {
  const [name, setName] = useState('');
  const [file, setFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!name || !file) {
      setError('Vyplňte název datasetu a vyberte soubor.');
      return;
    }
    
    try {
      setIsUploading(true);
      setError(null);
      setSuccessMessage(null);
      
      await uploadDataset(name, file);
      
      setSuccessMessage(`Dataset "${name}" byl úspěšně nahrán.`);
      setName('');
      setFile(null);
      
      // Přesetování formuláře (pro odstranění jména souboru)
      e.target.reset();
      
      if (onSuccess) {
        onSuccess();
      }
    } catch (err) {
        if (err.response !== undefined) {
          console.log(err.response.data)
          setError('Chyba při nahrávání datasetu: ' + err.response.data);
        } else {
          console.log(err.message)
          setError('Chyba při nahrávání datasetu: ' + err.message);
        }
      
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="card">
      <div className="card-body">
        {error && <div className="alert alert-danger">{error}</div>}
        {successMessage && <div className="alert alert-success">{successMessage}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label htmlFor="datasetName" className="form-label">Název datasetu</label>
            <input
              type="text"
              className="form-control"
              id="datasetName"
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={isUploading}
              required
            />
          </div>
          
          <div className="mb-3">
            <label htmlFor="datasetFile" className="form-label">Soubor s daty</label>
            <input
              type="file"
              className="form-control"
              id="datasetFile"
              onChange={(e) => setFile(e.target.files[0])}
              disabled={isUploading}
              required
            />
            <div className="form-text">Soubor musí být ve formátu txt s ID uživatelů oddělených mezerou na každém řádku.</div>
          </div>
          
          <button type="submit" className="btn btn-primary" disabled={isUploading}>
            {isUploading ? 'Nahrávání...' : 'Nahrát dataset'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default DatasetUpload;
