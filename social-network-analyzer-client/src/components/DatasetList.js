import React from 'react';

const DatasetList = ({ datasets, onSelectDataset, selectedDatasetId }) => {
  if (datasets.length === 0) {
    return <p>Žádné datasety nejsou k dispozici.</p>;
  }

  return (
    <div className="list-group">
      {datasets.map(dataset => (
        <button
          key={dataset.id}
          className={`list-group-item list-group-item-action ${selectedDatasetId === dataset.id ? 'active' : ''}`}
          onClick={() => onSelectDataset(dataset)}
        >
          <div className="d-flex w-100 justify-content-between">
            <h5 className="mb-1">{dataset.name}</h5>
            <small>ID: {dataset.id}</small>
          </div>
          <small>Vytvořeno: {new Date(dataset.createdAt).toLocaleString()}</small>
        </button>
      ))}
    </div>
  );
};

export default DatasetList;
