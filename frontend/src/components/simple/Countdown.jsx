import React, { useState, useEffect } from "react";
import "../../styles/countdown.css";

const CountdownTimer = ({ initialTime = 120, onComplete }) => {
  const [countdownTime, setCountdownTime] = useState(initialTime);

  useEffect(() => {
    const countdownInterval = setInterval(() => {
      setCountdownTime((prevTime) => {
        if (prevTime <= 1) {
          clearInterval(countdownInterval);
          if (onComplete) onComplete();
          return 0;
        }
        return prevTime - 1;
      });
    }, 1000);

    return () => clearInterval(countdownInterval);
  }, [initialTime]);

  const formatTime = (time) => {
    const minutes = Math.floor(time / 60);
    const seconds = time % 60;
    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
  };

  return (
    <div className="countdown-container">
      <div className="countdown-header">Next Submission Available In:</div>
      <div className="countdown-timer">{formatTime(countdownTime)}</div>
      <div className="countdown-message">Please wait before submitting again.</div>
    </div>
  )
}
export default CountdownTimer;