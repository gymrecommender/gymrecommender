import React, { useState } from 'react';
import Slider from "./Slider.jsx";
import Button from "./Button.jsx";
const FeedbackForm = () => {
    const [formValue, setFormValue] = useState({
        congestionRatingWait: "no waiting",
        congestionRatingCrowd: "nearly empty",
        timeOfVisit: "",
        overallRating: 3,
        travelTime: "",
        travelCost: "",
        gymPrice: ""
    });

    const handleChange = (name, value) => {
        setFormValue({ ...formValue, [name]: value });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        console.log(formValue);
    };

    return (
        <aside className="sliders">
            <h3>Share your experience with others!</h3>

            <label>Congestion Rating: average waiting time for a machine or space</label>
            <select
                value={formValue.congestionRatingWait}
                onChange={(e) => handleChange("congestionRatingWait", e.target.value)}
            >
                <option value="no waiting">No Waiting</option>
                <option value="less than 5 minutes">Less than 5 Minutes</option>
                <option value="5-10 minutes">5-10 Minutes</option>
                <option value="10-20 minutes">10-20 Minutes</option>
                <option value="more than 20 minutes">More than 20 Minutes</option>
            </select>

            <label>Congestion Rating: how crowded the gym feels</label>
            <select
                value={formValue.congestionRatingCrowd}
                onChange={(e) => handleChange("congestionRatingCrowd", e.target.value)}
            >
                <option value="nearly empty">Nearly Empty</option>
                <option value="lightly busy">Lightly Busy</option>
                <option value="moderately busy">Moderately Busy</option>
                <option value="busy but manageable">Busy but Manageable</option>
                <option value="very crowded">Very Crowded</option>
            </select>

            <label>Time of Visit:</label>
            <input
                type="text"
                placeholder="Enter time of visit"
                value={formValue.timeOfVisit}
                onChange={(e) => handleChange("timeOfVisit", e.target.value)}
            />

            <label>Overall Rating:</label>
            <Slider
                type={"range"}
                name="overallRating"
                min={1}
                max={5}
                step={0.5}
                value={formValue.overallRating}
                onChange={(value) => handleChange("overallRating", value)}
            />

            <label>Traveling Time:</label>
            <input
                type="number"
                placeholder="Traveling time in minutes"
                value={formValue.travelTime}
                onChange={(e) => handleChange("travelTime", e.target.value)}
            />

            <label>Traveling Cost:</label>
            <input
                type="text"
                placeholder="Total traveling cost in local currency"
                value={formValue.travelCost}
                onChange={(e) => handleChange("travelCost", e.target.value)}
            />

            <label>Gym Price:</label>
            <input
                type="text"
                placeholder="Gym price cost in local currency"
                value={formValue.gymPrice}
                onChange={(e) => handleChange("gymPrice", e.target.value)}
            />

            <Button type={"submit"} onClick={handleSubmit}>Apply</Button>
        </aside>
    );
};

export default FeedbackForm;
