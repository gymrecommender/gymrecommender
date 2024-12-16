import {useFormContext} from "react-hook-form";
import classNames from "classnames";
import {generateValidationRules} from "../../services/helpers.jsx";

const Select = ({className, wClassName, isBorderError, required, data, label, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();

	const options = data?.map((item, index) => {
		return (
			<option value={item.value} key={index}>
				{item.label}
			</option>
		);
	})
	return (
		<div className={classNames('selector', wClassName, required ? "required" : '')}>
			{label ? (<label>{label}</label>) : ''}
			{!isBorderError && errors[name] ? <span className={"input-field-error"}>{errors[name].message}</span> : ""}
			<select
				{...register(name, generateValidationRules(label, {required}))}
				{...rest}
				className={classNames(className, isBorderError && errors[name] ? "error-border" : null)}
			>
				{options ?? ""}
			</select>
		</div>
	)
}

export default Select;