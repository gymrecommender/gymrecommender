import classNames from 'classnames';
import {useFormContext} from "react-hook-form";
import {generateValidationRules} from "../../services/helpers.jsx";

const Input = ({children, wClassName, label, className, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();
	const {max, min, minLength, maxLength} = rest
	const {sameAs, pattern, required, ...inputParams} = rest;

	return (
		<div className={classNames('input-field', wClassName, required ? "required" : "")}>
			{label ?
				<label className={`input-field-label`} htmlFor={name}>
					{label}
				</label> : null}
			{children}
			{errors[name] ? <span className={"input-field-error"}>{errors[name].message}</span> : ""}
			<input
				{...inputParams}
				{...(className && {className})}//conditional rendering of the class
				{...register(name, generateValidationRules(label, {max, min, required, sameAs, minLength, maxLength, pattern}))}
			/>
		</div>
	)
}

export default Input;