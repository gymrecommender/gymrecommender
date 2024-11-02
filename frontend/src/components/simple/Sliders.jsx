import React, { useState } from 'react';

const Sliders = () => {
  const [timeValue, setTimeValue] = useState(30);
  const [tPriceValue, setTPriceValue] = useState(25);
  const [mPriceValue, setMPriceValue] = useState(50);
  const [ratingValue, setRatingValue] = useState(3);
  const [congestionValue, setCongestionValue] = useState(3);

  const handleSliderChange = (setFunction) => (event) => {
    setFunction(event.target.value);
  };

  return (
    <section className="sliders">
      <h3>Tell us your preferences!</h3>

      {/* Traveling Time Slider */}
      <label>
        Traveling Time: max <span>{timeValue} min</span>
      </label>
      <input
        type="range"
        min="1"
        max="120"
        value={timeValue}
        onChange={handleSliderChange(setTimeValue)}
      />

      {/* Traveling Price Slider */}
      <label>
        Traveling Price: max <span>{tPriceValue} €</span>
      </label>
      <input
        type="range"
        min="0"
        max="50"
        value={tPriceValue}
        onChange={handleSliderChange(setTPriceValue)}
      />

      {/* Membership Price Slider */}
      <label>
        Membership Price: max <span>{mPriceValue} €</span>
      </label>
      <input
        type="range"
        min="0"
        max="300"
        value={mPriceValue}
        onChange={handleSliderChange(setMPriceValue)}
      />

      {/* Overall Rating Slider */}
      <label>
        Overall Rating: min <span>{ratingValue} *</span>
      </label>
      <input
        type="range"
        min="1"
        max="5"
        step="0.5"
        value={ratingValue}
        onChange={handleSliderChange(setRatingValue)}
      />

      {/* Congestion Rating Slider */}
      <label>
        Congestion Rating: min <span>{congestionValue} *</span>
      </label>
      <input
        type="range"
        min="1"
        max="5"
        step="0.5"
        value={congestionValue}
        onChange={handleSliderChange(setCongestionValue)}
      />

      <button>Apply</button>
    </section>
  );
};

export default Sliders;
