import React, { useEffect,useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './RegisterPage.css';

export default function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();
  // Preventing the user from getting back to register page  page after registration
  useEffect(() => {
  const userId = localStorage.getItem('userId');
  if (userId) {
    navigate('/buildings');
  }
}, []);
  const handleRegister = async () => {
  try {
    const res = await axios.post('http://localhost:5285/api/users/register', {
      email,
      password
    });
    localStorage.setItem('userId', res.data.id);
    navigate('/buildings');
  } catch (err: any) {
  if (axios.isAxiosError(err) && err.response?.data) {
    const message =
      typeof err.response.data === 'string'
        ? err.response.data
        : err.response.data.error || err.response.data.title || 'שגיאת שרת כללית';
    alert(`שגיאה: ${message}`);
  } else {
    alert('שגיאה לא צפויה בעת ההרשמה');
  }
  console.error('Registration failed:', err);
}
};

  return (
    <div className="page-container">
      <div className="card">
        <h2>הרשמה</h2>

        <div>
          <label className="label">אימייל</label>
          <input
            type="email"
            className="input"
            placeholder="your@email.com"
            onChange={e => setEmail(e.target.value)}
          />
        </div>

        <div>
          <label className="label">סיסמה</label>
          <input
            type="password"
            className="input"
            placeholder="••••••••"
            onChange={e => setPassword(e.target.value)}
          />
        </div>

        <button onClick={handleRegister} className="button">
          הרשם
        </button>
        <p style={{ textAlign: 'center', marginTop: '1rem' }}>
  כבר יש לך חשבון? <span style={{ color: '#2563eb', cursor: 'pointer' }} onClick={() => navigate('/login')}>התחבר כאן</span>
</p>
      </div>
    </div>
  );
}
