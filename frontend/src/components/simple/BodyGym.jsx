import React, { useState } from 'react';
import Input from './Input.jsx';
import Button from './Button.jsx';
import LocationControls from './LocationControls.jsx';
import Map from './Map.jsx';

const BodyGym = () => {
  const [lon, setLon] = useState(0);
  const [lat, setLat] = useState(0);

  const handleSubmitSearch = (value) => {
    console.log(value);
  };

  const handleGetLocation = () => {
    console.log("Get location");
    // Add logic to get current location here
  };

  return (
    <div className="section-body">
      <h2>Manage Your Gym Account: </h2>

      {/* Request to Own a Gym Form */}
      <div className="section section-request">
        <h3>Request to Own a Gym:</h3>
        <form className="request-form">
            <div className="form-row">
                <div className="form-item">
                    <label htmlFor="gym-name">Gym Name:</label>
                    <Input
                        type="text"
                        name="gym-name"
                        placeholder="Enter gym name"
                        className="input-box"
                        required
                    />
                </div>
                <div className="form-item">
                    <label htmlFor="gym-location">Gym Location:</label>
                    <LocationControls onGetLocation={handleGetLocation} onSubmitSearch={handleSubmitSearch} />
                    <Map coordinates={{ lon, lat }} />
                    <Button type="submit" className="button-submit">Submit Request</Button>
                </div>
            </div>
            
            
        </form>
      </div>

      {/* List of Managed Gyms */}
      <div className="section section-managed-gyms">
        <h3>List of Managed Gyms:</h3>
        <div className="accordion-item">
          <button className="accordion-button">Gym 1</button>
          <div className="accordion-content">
            <p>Details about Gym 1...</p>
            <form className="update-form">
              <label htmlFor="update-info-1">Update Gym Information:</label>
              <Input
                type="text"
                name="update-info-1"
                placeholder="Update details"
                className="input-box"
              />
              <Button type="submit" className="button-submit">Update</Button>
            </form>
          </div>
        </div>

        <div className="accordion-item">
          <button className="accordion-button">Gym 2</button>
          <div className="accordion-content">
            <p>Details about Gym 2...</p>
            <form>
              <label htmlFor="update-info-2">Update Gym Information:</label>
              <input type="text" id="update-info-2" name="update-info-2" placeholder="Update details" />
              <button type="submit" className="button-submit">Update</button>
            </form>
          </div>
        </div>
      </div>

      {/* Marking Gyms as Unavailable */}
      <div className="section section-unavailable">
        <h3>Mark Gym as Unavailable: </h3>
        <form className="unavailable-form">
          <label htmlFor="select-gym">Select Gym:</label>
          <select id="select-gym" name="select-gym" className="input-box">
            <option value="gym1">Gym 1</option>
            <option value="gym2">Gym 2</option>
          </select>

          <div className="time">
            <label htmlFor="unavailable-from">Unavailable From:</label>
            <Input type="time" id="unavailable-from" name="unavailable-from" className="input-box" required />

            <label htmlFor="unavailable-to">Unavailable To:</label>
            <Input type="time" id="unavailable-to" name="unavailable-to" className="input-box" required />
          </div>

          <Button type="submit" className="button-submit">Mark as Unavailable</Button>
        </form>
      </div>

      
    </div>
  );
};

export default BodyGym;
