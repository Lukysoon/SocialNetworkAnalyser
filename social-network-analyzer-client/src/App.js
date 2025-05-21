import React, { useState, useEffect } from 'react';
import './App.css';
import DatasetList from './components/DatasetList';
import DatasetUpload from './components/DatasetUpload';
import DatasetStatistics from './components/DatasetStatistics';
import { fetchDatasets } from './services/datasetService';

function App() {
  const [datasets, setDatasets] = useState([]);
  const [selectedDataset, setSelectedDataset] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loadDatasets = async () => {
    try {
      setLoading(true);
      const data = await fetchDatasets();
      setDatasets(data);
      setError(null);
    } catch (err) {
      console.log(err)
      setError('Nepodařilo se načíst datasety: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDatasets();
  }, []);

  const handleDatasetUploadSuccess = () => {
    loadDatasets();
  };

  const handleSelectDataset = (dataset) => {
    setSelectedDataset(dataset);
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Analyzátor sociální sítě</h1>
      </header>
      
      <main>
        <div className="container">
          {error && <div className="error-message">{error}</div>}
          
          <div className="row">
            <div className="col">
              <h2>Nahrát nová data</h2>
              <DatasetUpload onSuccess={handleDatasetUploadSuccess} />
            </div>
          </div>
          
          <div className="row mt-4">
            <div className="col-md-4">
              <h2>Dostupné datasety</h2>
              {loading ? (
                <p>Načítání...</p>
              ) : (
                <DatasetList 
                  datasets={datasets} 
                  onSelectDataset={handleSelectDataset}
                  selectedDatasetId={selectedDataset?.id}
                />
              )}
            </div>
            
            <div className="col-md-8">
              <h2>Statistiky datasetu</h2>
              {selectedDataset ? (
                <DatasetStatistics datasetId={selectedDataset.id} />
              ) : (
                <p>Vyberte dataset ze seznamu pro zobrazení statistik</p>
              )}
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}

export default App;
