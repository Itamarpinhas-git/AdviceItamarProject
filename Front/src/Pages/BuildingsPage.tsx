import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './Buildings.css';

interface Building {
  id: string;
  name: string;
  numberOfFloors: number;
}

export default function BuildingsPage() {
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [newName, setNewName] = useState('');
  const [newFloors, setNewFloors] = useState(1);
  const navigate = useNavigate();

  useEffect(() => {
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    axios.get(`http://localhost:5285/api/buildings/by-user/${userId}`)
      .then(res => setBuildings(res.data))
      .catch(err => console.error('Failed to load buildings:', err));
  }, []);

  // Add building handler
  const handleAddBuilding = async (e: React.FormEvent) => {
    e.preventDefault();
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    try {
      const res = await axios.post('http://localhost:5285/api/buildings/AddBuilding', {
        name: newName,
        numberOfFloors: newFloors,
        userId
      });
      setBuildings([...buildings, res.data]);
      setNewName('');
      setNewFloors(1);
    } catch (err: any) {
  if (axios.isAxiosError(err) && err.response?.data) {
    const message =
      typeof err.response.data === 'string'
        ? err.response.data
        : err.response.data.error || 'שגיאה לא צפויה';
    alert(`שגיאה: ${message}`);
  } else {
    alert('קרתה שגיאה כללית בעת הוספת הבניין.');
  }

  console.error('Failed to add building:', err);
}
  };

  return (
    <div className="buildings-container">
      <h2 className="buildings-title">הבניינים שלי</h2>

      {/* Add Building Form */}
      <form className="add-building-form" onSubmit={handleAddBuilding}>
        <input
          type="text"
          placeholder="שם בניין"
          value={newName}
          onChange={e => setNewName(e.target.value)}
          required
        />
        <input
          type="number"
          placeholder="מספר קומות"
          value={newFloors}
          min={1}
          onChange={e => setNewFloors(Number(e.target.value))}
          required
        />
        <button type="submit">הוסף בניין</button>
      </form>

      <div className="buildings-grid">
        {buildings.map(b => (
          <div key={b.id} className="building-card">
            <div>
              <h3 className="building-title">{b.name}</h3>
              <p className="building-info">מספר קומות: {b.numberOfFloors}</p>
            </div>
            <button
              onClick={() => navigate(`/buildings/${b.id}`)}
              className="building-button"
            >
              עבור לסימולציה
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}