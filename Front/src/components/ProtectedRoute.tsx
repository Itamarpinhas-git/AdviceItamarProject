import React, { ReactElement } from 'react';
import { Navigate } from 'react-router-dom';

export default function ProtectedRoute({ children }: { children: ReactElement }) {
  const userId = localStorage.getItem('userId');

  if (!userId) {
    return <Navigate to="/login" replace />;
  }

  return children;
}
