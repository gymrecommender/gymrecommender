import classNames from 'classnames';
import {useFormContext} from "react-hook-form";
import {generateValidationRules} from "../../services/helpers.jsx";

const Input = ({children, wClassName, label, isBorderError, className, showAsterisks=true, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();
	const {max, min, minLength, maxLength} = rest
	const {sameAs, pattern, required, ...inputParams} = rest;

	return (
		<div className={classNames('input-field', wClassName, required && showAsterisks ? "required" : "")}>
			{label ?
				<label className={`input-field-label`} htmlFor={name}>
					{label}
				</label> : null}
			{children}
			{!isBorderError && errors[name] ? <span className={"input-field-error"}>{errors[name].message}</span> : ""}
			<input
				{...inputParams}
				className={classNames(className, isBorderError && errors[name] ? "error-border" : null)}
				{...register(name, generateValidationRules(label, {max, min, required, sameAs, minLength, maxLength, pattern}))}
			/>
		</div>
	)
}

export default Input;