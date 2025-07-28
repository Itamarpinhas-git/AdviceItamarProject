import React, { useState,useEffect  } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './RegisterPage.css'; // 👈 אותו CSS כמו ב־RegisterPage

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();
  // priventing the user to get back to login page after login
  useEffect(() => {
  const userId = localStorage.getItem('userId');
  if (userId) {
    navigate('/buildings');
  }
}, []);

  const handleLogin = async () => {
    try {
      const res = await axios.post('http://localhost:5285/api/users/login', { email, password });
     localStorage.setItem('userId', res.data.id); 
      navigate('/buildings');
    } catch (err) {
      console.error('Login failed', err);
    }
  };

  return (
    <div className="page-container">
      <div className="card">
        <h2>התחברות</h2>

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

        <button onClick={handleLogin} className="button">
          התחבר
        </button>
        <p style={{ textAlign: 'center', marginTop: '1rem' }}>
  אין לך חשבון? <span style={{ color: '#2563eb', cursor: 'pointer' }} onClick={() => navigate('/register')}>הירשם כאן</span>
</p>

      </div>
    </div>
  );
}
