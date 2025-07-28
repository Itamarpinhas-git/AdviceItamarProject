import React, { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import axios from 'axios';
import { useParams } from 'react-router-dom';
import './Buildings.css';

interface ElevatorData {
  elevatorId: number;
  floor: number;
  status: string;
  direction: string;
  isAtFloor: boolean;
  doorStatus?: string;
  remainingDoorTime?: number; // Not used anymore for countdown, but kept for compatibility
}

export default function BuildingView() {
  const { id: buildingId } = useParams();
  const [elevator, setElevator] = useState<ElevatorData>({
    elevatorId: 0,
    floor: 0,
    status: 'עומדת',
    direction: '',
    isAtFloor: false,
    doorStatus: 'Closed'
  });

  const [elevatorId, setElevatorId] = useState<number | null>(null);
  const [selectTarget, setSelectTarget] = useState(false);
  const [totalFloors, setTotalFloors] = useState<number>(1);
  const [building, setBuilding] = useState<any>(null);
  const [doorTimeout, setDoorTimeout] = useState<number>(0);
  
  // Ref to store the interval ID for cleanup
  const countdownIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Client-side countdown timer effect
  useEffect(() => {
    // Clear any existing interval
    if (countdownIntervalRef.current) {
      clearInterval(countdownIntervalRef.current);
      countdownIntervalRef.current = null;
    }

    // Start countdown only when doors are open
    if (selectTarget && doorTimeout > 0) {
      console.log(`⏰ Starting client-side countdown from ${doorTimeout} seconds`);
      
      countdownIntervalRef.current = setInterval(() => {
        setDoorTimeout((prev) => {
          const newTime = prev - 1;
          console.log(`⏳ Countdown: ${newTime} seconds remaining`);
          
          if (newTime <= 0) {
            console.log('🚪 Countdown reached 0 - doors should close');
            // Clear the interval when countdown reaches 0
            if (countdownIntervalRef.current) {
              clearInterval(countdownIntervalRef.current);
              countdownIntervalRef.current = null;
            }
            return 0;
          }
          
          return newTime;
        });
      }, 1000);
    }

    // Cleanup function
    return () => {
      if (countdownIntervalRef.current) {
        clearInterval(countdownIntervalRef.current);
        countdownIntervalRef.current = null;
      }
    };
  }, [selectTarget, doorTimeout]); // Re-run when selectTarget changes

  // SignalR setup
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5285/elevatorHub')
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error(' SignalR Error:', err));

    // Listen for elevator updates
    connection.on('ReceiveElevatorUpdate', (data: ElevatorData) => {
      console.log('🔄 Received elevator update:', data);
      console.log('📊 status:', data.status);
      
      if (elevatorId && data.elevatorId === elevatorId) {
        setElevator(data);
        
        // 🔑 KEY: Server controls when to show/hide destination selection
        const shouldShowTargets = data.status === 'דלתות פתוחות';
        
        if (shouldShowTargets && !selectTarget) {
          //  Doors just opened - start client-side countdown
          console.log(' Doors opened - starting 10-second countdown');
          setSelectTarget(true);
          setDoorTimeout(10); // Start from 10 seconds
        } else if (!shouldShowTargets && selectTarget) {
          //  Doors closed or status changed - stop countdown immediately
          console.log(' Doors closed or status changed - stopping countdown');
          setSelectTarget(false);
          setDoorTimeout(0);
          
          // Clear any running interval
          if (countdownIntervalRef.current) {
            clearInterval(countdownIntervalRef.current);
            countdownIntervalRef.current = null;
          }
        }
      }
    });

    // Get building and elevator data
    fetchBuildingData();

    return () => {
      // Cleanup SignalR connection and any running intervals
      connection.stop();
      if (countdownIntervalRef.current) {
        clearInterval(countdownIntervalRef.current);
        countdownIntervalRef.current = null;
      }
    };
  }, [buildingId, elevatorId, selectTarget]); // Added selectTarget to dependencies

  const fetchBuildingData = async () => {
    try {
      const response = await axios.get(`http://localhost:5285/api/buildings/${buildingId}`);
      setBuilding(response.data);
      setTotalFloors(response.data.numberOfFloors);
      
      // Get elevator for this building
      const elevatorsResponse = await axios.get(`http://localhost:5285/api/elevators/by-building/${buildingId}`);
      if (elevatorsResponse.data && elevatorsResponse.data.length > 0) {
        const elevatorData = elevatorsResponse.data[0];
        setElevatorId(elevatorData.id);
        
        setElevator({
          elevatorId: elevatorData.id,
          floor: elevatorData.currentFloor,
          status: getHebrewStatus(elevatorData.status),
          direction: getDirectionSymbol(elevatorData.direction),
          isAtFloor: false,
          doorStatus: elevatorData.doorStatus || 'Closed'
        });
      }
    } catch (err) {
      console.error('Failed to fetch building data:', err);
    }
  };

  const getHebrewStatus = (status: string) => {
    switch (status) {
      case 'Idle': return 'עומדת';
      case 'MovingUp': return 'עולה';
      case 'MovingDown': return 'יורדת';
      case 'OpeningDoors': return 'דלתות פתוחות';
      case 'ClosingDoors': return 'דלתות נסגרות';
      default: return status;
    }
  };

  const getDirectionSymbol = (direction: string) => {
    switch (direction) {
      case 'Up': return '▲';
      case 'Down': return '▼';
      default: return '';
    }
  };

  // External elevator call
  const handleCallElevator = async (floor: number, direction: 'up' | 'down') => {
    try {
      console.log(` Calling elevator to floor ${floor}`);
      
      const response = await axios.post('http://localhost:5285/api/elevatorcalls/createCall', {
        buildingId: Number(buildingId),
        requestedFloor: floor,
        destinationFloor: null
      });

      console.log('✅ Call response:', response.data);
      alert(`מעלית נקראה לקומה ${floor} - המעלית תגיע בקרוב`);
    } catch (err: any) {
      const message = err?.response?.data?.error || 'שגיאה בשליחת הקריאה למעלית';
      alert(message);
      console.error('Elevator call failed:', err);
    }
  };

  // Inside elevator destination selection
  const handleChooseTarget = async (targetFloor: number) => {
    if (!elevatorId) {
      alert("מזהה המעלית לא נטען עדיין");
      return;
    }

    try {
      console.log(`Selecting destination floor ${targetFloor} for elevator ${elevatorId}`);
      
      const response = await axios.post('http://localhost:5285/api/elevatorcalls/insideCall', {
        elevatorId: elevatorId,
        destinationFloor: targetFloor
      });

      console.log('✅ Inside call response:', response.data);
      
      // Immediately stop the countdown and hide destination selection
      setSelectTarget(false);
      setDoorTimeout(0);
      
      // Clear the countdown interval
      if (countdownIntervalRef.current) {
        clearInterval(countdownIntervalRef.current);
        countdownIntervalRef.current = null;
      }
      
      alert(` נבחרה קומה ${targetFloor} - המעלית סוגרת דלתות ותתחיל לנוע`);
    } catch (err: any) {
      console.error(' Choose target failed:', err);
      const message = err?.response?.data?.error || err?.response?.data || 'שגיאה בבחירת קומת יעד';
      alert(` ${message}`);
    }
  };

  if (!building) {
    return (
      <div className="page-container">
        <div className="title">טוען נתוני בניין...</div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h2 className="title">{building.name} - סימולציית מעלית</h2>
      
      <div className="building-info" style={{ textAlign: 'center', marginBottom: '20px' }}>
        <p><strong>קומות:</strong> {totalFloors}</p>
        <p><strong>מעלית:</strong> 
          <span style={{ 
            color: elevator.status === 'עולה' || elevator.status === 'יורדת' ? '#dc2626' : '#059669',
            fontWeight: 'bold'
          }}>
            {elevator.status}
          </span> 
          בקומה {elevator.floor} {elevator.direction}
        </p>
      </div>

      <div className="building">
        {/* Animated elevator */}
        <div
          className="elevator-box"
          style={{
            transform: `translateY(${(totalFloors - 1 - elevator.floor) * 70}px)`,
            transition: 'transform 2s ease-in-out',
            backgroundColor: elevator.isAtFloor ? '#10b981' : '#2563eb',
            fontSize: '16px',
            fontWeight: 'bold'
          }}
        >
          🛗 {elevator.floor}
        </div>
        
        {/* Building floors */}
        {[...Array(totalFloors)].map((_, i) => {
          const floor = totalFloors - 1 - i; // Reverse order for display
          const isHere = elevator.floor === floor;

          return (
            <div key={floor} className="floor">
              <div className="floor-label">קומה {floor}</div>
              <div className="floor-indicator">
                {isHere && (
                  <div className="elevator-here">
                    {elevator.status} {elevator.direction}
                  </div>
                )}
              </div>
              <div className="call-buttons">
                {floor < totalFloors - 1 && (
                  <button
                    className="call-btn"
                    onClick={() => handleCallElevator(floor, 'up')}
                    disabled={isHere}
                  >
                    הזמן מעלית ▲
                  </button>
                )}
                {floor > 0 && (
                  <button
                    className="call-btn"
                    onClick={() => handleCallElevator(floor, 'down')}
                    disabled={isHere}
                  >
                    הזמן מעלית ▼
                  </button>
                )}
              </div>
            </div>
          );
        })}

        {/* CORE FEATURE: Destination selection when doors are open */}
        {selectTarget && (
          <div className="target-section">
            <h3 className="target-title"> המעלית פתוחה - בחר את היעד שלך:</h3>
            <p style={{ fontSize: '14px', color: '#666', marginBottom: '10px' }}>
              אתה נמצא בקומה {elevator.floor}. לחץ על הקומה שאליה תרצה לנסוע.
            </p>
            
            {/* Client-side countdown timer */}
            {doorTimeout > 0 && (
              <div style={{ 
                background: doorTimeout <= 3 ? '#fef3c7' : '#dbeafe', 
                padding: '12px 20px', 
                borderRadius: '8px', 
                marginBottom: '15px',
                border: doorTimeout <= 3 ? '2px solid #f59e0b' : '2px solid #3b82f6',
                textAlign: 'center',
                transition: 'all 0.3s ease',
                transform: doorTimeout <= 3 ? 'scale(1.02)' : 'scale(1)',
                boxShadow: doorTimeout <= 3 ? '0 4px 12px rgba(220, 38, 38, 0.2)' : '0 2px 8px rgba(59, 130, 246, 0.1)'
              }}>
                <div style={{ 
                  fontSize: '24px', 
                  fontWeight: 'bold',
                  color: doorTimeout <= 3 ? '#dc2626' : '#1e40af',
                  marginBottom: '4px',
                  fontFamily: 'monospace'
                }}>
                  ⏰ {doorTimeout}
                </div>
                <p style={{ 
                  margin: 0, 
                  fontSize: '14px', 
                  color: doorTimeout <= 3 ? '#dc2626' : '#1e40af',
                  fontWeight: doorTimeout <= 3 ? 'bold' : 'normal'
                }}>
                  {doorTimeout <= 3 
                    ? ' מהר! הדלתות נסגרות!' 
                    : 'הדלתות יסגרו בעוד'
                  } {doorTimeout === 1 ? 'שנייה' : 'שניות'}
                </p>

                {/* Visual progress bar */}
                <div style={{
                  width: '100%',
                  height: '6px',
                  backgroundColor: '#e5e7eb',
                  borderRadius: '3px',
                  marginTop: '8px',
                  overflow: 'hidden'
                }}>
                  <div style={{
                    width: `${(doorTimeout / 10) * 100}%`,
                    height: '100%',
                    backgroundColor: doorTimeout <= 3 ? '#dc2626' : '#1e40af',
                    transition: 'width 1s linear',
                    borderRadius: '3px'
                  }} />
                </div>
              </div>
            )}

            {/* Debug info */}
            <div style={{ 
              fontSize: '12px', 
              color: '#666', 
              marginBottom: '15px', 
              padding: '8px', 
              backgroundColor: '#f8f9fa', 
              borderRadius: '4px',
              border: '1px solid #e9ecef'
            }}>
               Debug: doorTimeout = {doorTimeout}, selectTarget = {selectTarget.toString()}, status = {elevator.status}
              <br />
              Client-side countdown: {doorTimeout > 0 ? 'RUNNING' : 'STOPPED'}
            </div>
            
            <div className="target-buttons">
              {[...Array(totalFloors)].map((_, i) => {
                const floor = i;
                if (floor === elevator.floor) return null;
                return (
                  <button
                    key={floor}
                    className="target-btn"
                    onClick={() => handleChooseTarget(floor)}
                    style={{
                      opacity: doorTimeout <= 3 ? 1 : 0.9,
                      transform: doorTimeout <= 3 ? 'scale(1.05)' : 'scale(1)',
                      transition: 'all 0.3s ease',
                      animation: doorTimeout <= 3 ? 'pulse 1s infinite' : 'none'
                    }}
                  >
                    קומה {floor}
                  </button>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}