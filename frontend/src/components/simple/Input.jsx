import {useFormContext} from "react-hook-form";
import {generateValidationRules} from "../../services/helpers.jsx";

const Input = ({wClassName, label, className, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();
	const {max, min, required, minLength, maxLength} = rest
	const {sameAs, pattern, ...inputParams} = rest;

	return (
		<div className={`input-field ${wClassName ?? ''}`}>
			{label ?
				<label className={`input-field-label`} htmlFor={name}>
					{label}{rest.required ? <span>*</span> : ''}
				</label> : null}
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