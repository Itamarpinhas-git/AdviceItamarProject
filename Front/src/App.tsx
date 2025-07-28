import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './Pages/LoginPage';
import RegisterPage from './Pages/RegisterPage';
import BuildingsPage from './Pages/BuildingsPage';
import BuildingViewPage from './Pages/BuildingViewPage';
import Navbar from './components/Navbar';
import ProtectedRoute from './components/ProtectedRoute'; 

export default function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        {/* Default route - redirects to register */}
        <Route path="/" element={<Navigate to="/register" replace />} />
        
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="/buildings"
          element={
            <ProtectedRoute>
              <BuildingsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/buildings/:id"
          element={
            <ProtectedRoute>
              <BuildingViewPage />
            </ProtectedRoute>
          }
        />
        
        {/* Catch all route - redirects unknown paths to register */}
        <Route path="*" element={<Navigate to="/register" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
