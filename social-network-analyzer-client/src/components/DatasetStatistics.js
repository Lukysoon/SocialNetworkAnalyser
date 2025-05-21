import React, { useState, useEffect } from 'react';
import { fetchDatasetStatistics } from '../services/datasetService';
import { Bar } from 'react-chartjs-2';
import { Chart, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js';

// Registrace potřebných Chart.js komponent
Chart.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const DatasetStatistics = ({ datasetId }) => {
  const [statistics, setStatistics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const loadStatistics = async () => {
      try {
        setLoading(true);
        const data = await fetchDatasetStatistics(datasetId);
        setStatistics(data);
        setError(null);
      } catch (err) {
        if (err.response !== undefined) {
          setError('Nepodařilo se načíst statistiky: ' + err.response.data);
        } else {
          setError('Nepodařilo se načíst statistiky: ' + err.message);
        }
        
        setStatistics(null);
      } finally {
        setLoading(false);
      }
    };

    if (datasetId) {
      loadStatistics();
    }
  }, [datasetId]);

  if (!datasetId) {
    return <p>Vyberte dataset pro zobrazení statistik.</p>;
  }

  if (loading) {
    return <p>Načítání statistik...</p>;
  }

  if (error) {
    return <div className="alert alert-danger">{error}</div>;
  }

  if (!statistics) {
    return <p>Žádné statistiky nejsou k dispozici.</p>;
  }

  // Data pro graf
  const chartData = {
    labels: ['Celkový počet uživatelů', 'Průměrný počet přátel na uživatele'],
    datasets: [
      {
        label: 'Hodnota',
        data: [statistics.totalUsers, statistics.averageFriendsPerUser],
        backgroundColor: ['rgba(54, 162, 235, 0.5)', 'rgba(75, 192, 192, 0.5)'],
        borderColor: ['rgb(54, 162, 235)', 'rgb(75, 192, 192)'],
        borderWidth: 1,
      }
    ]
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  return (
    <div className="card">
      <div className="card-body">
        <h3 className="card-title">Statistiky datasetu (ID: {datasetId})</h3>
        
        <div className="row mt-3">
          <div className="col-md-6">
            <div className="card">
              <div className="card-body">
                <h4 className="card-title">Celkový počet uživatelů</h4>
                <p className="display-4 text-center">{statistics.totalUsers}</p>
              </div>
            </div>
          </div>
          
          <div className="col-md-6">
            <div className="card">
              <div className="card-body">
                <h4 className="card-title">Průměrný počet přátel na uživatele</h4>
                <p className="display-4 text-center">
                  {statistics.averageFriendsPerUser.toFixed(2)}
                </p>
              </div>
            </div>
          </div>
        </div>
        
        <div className="mt-4" style={{ height: '300px' }}>
          <Bar data={chartData} options={chartOptions} />
        </div>
      </div>
    </div>
  );
};

export default DatasetStatistics;
