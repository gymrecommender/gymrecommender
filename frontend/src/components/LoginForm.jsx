import React, { useState } from 'react';
import {useNavigate} from "react-router-dom";
import Button from "./simple/Button.jsx";
import Input from "./simple/Input.jsx";

const LoginForm = () => {
    const [formValues, setFormValues] = useState({
        username: '',
        email: '',
        password: ''
    })
    const navigate = useNavigate();

    const handleSubmit = (e) => {
        e.preventDefault();
        // TODO: Handle login logic here

        navigate(`/account/${username}`);
    };

    const handleChange = (name, value) => {
        setFormValues({...formValues, [name]: value});
    }

    return (
        <div className="form-login">
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <Input type="text"
                       label={"Username"}
                       name="username"
                       required
                       value={formValues["username"]}
                       wClassName={"form-group"}
                       className={"input-login"}
                       onChange={handleChange}/>

                <Input type="email"
                       label={"Email"}
                       name="email"
                       required
                       value={formValues["email"]}
                       wClassName={"form-group"}
                       className={"input-login"}
                       onChange={handleChange}/>

                <Input type="password"
                       label={"Password"}
                       name="password"
                       required
                       value={formValues["password"]}
                       wClassName={"form-group"}
                       className={"input-login"}
                       onChange={handleChange}/>

                <Button className={"btn-login"} type={"submit"} onSubmit={handleSubmit}>Log in</Button>
            </form>
        </div>
    );
};

export default LoginForm;
