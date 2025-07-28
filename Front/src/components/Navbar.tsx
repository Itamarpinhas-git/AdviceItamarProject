import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

export default function Navbar() {
  const navigate = useNavigate();
  const [isHover, setIsHover] = useState(false);

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  const isLoggedIn = !!localStorage.getItem('userId');
  
  if (!isLoggedIn) return null; // Hide navbar when not logged in

  return (
    <nav style={styles.nav}>
      <Link
        to="/buildings"
        style={{
          ...styles.link,
          ...(isHover ? styles.linkHover : {})
        }}
        onMouseEnter={() => setIsHover(true)}
        onMouseLeave={() => setIsHover(false)}
      >
        הבניינים שלי
      </Link>
      <button onClick={handleLogout} style={styles.logout}>התנתק</button>
    </nav>
  );
}

const styles = {
  nav: {
    display: 'flex',
    justifyContent: 'space-between',
    padding: '1rem 2rem',
    background: '#1e3a8a',
    color: 'white',
    alignItems: 'center',
    marginBottom: '20px'
  },
  link: {
    textDecoration: 'none',
    color: '#495057',
    fontWeight: '500',
    padding: '8px 12px',
    borderRadius: '4px',
    border: '1px solid #dee2e6', // ✨ Added border
    backgroundColor: '#ffffff',   // Optional: background color
    transition: 'all 0.2s ease',
    // Optional hover effect:
    ':hover': {
      borderColor: '#adb5bd',
      backgroundColor: '#f8f9fa'
    }
  },
   linkHover: {
    borderColor: '#adb5bd',
    backgroundColor: '#f8f9fa',
    opacity: 0.85
  }
  ,
  
  logout: {
    backgroundColor: '#ef4444',
    border: 'none',
    color: 'white',
    padding: '6px 14px',
    borderRadius: '6px',
    cursor: 'pointer'
  }
};
