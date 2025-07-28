import React from 'react';
import { useParams } from 'react-router-dom';
import BuildingView from './BuildingView';
import './building-view-page.css'; 

export default function BuildingViewPage() {
  const { id } = useParams();

  return (
    <div className="building-view-page">
      <h2 className="building-view-title">סימולציית בניין {id}</h2>
      <BuildingView />
    </div>
  );
}
