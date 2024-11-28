import {useFormContext} from "react-hook-form";
import classNames from "classnames";
import {generateValidationRules} from "../../services/helpers.jsx";

const Select = ({className, wClassName, required, data, label, name, ...rest}) => {
	const {register, formState: {errors}} = useFormContext();

	const options = data?.map((item, index) => {
		return (
			<option value={item.value} key={index}>
				{item.label}
			</option>
		);
	})
	return (
		<div className={classNames('selector', wClassName)}>
			{label ? (<label>{label}{required ? <span>*</span> : ''}</label>) : ''}
			{errors[name] ? <span className={"input-field-error"}>{errors[name].message}</span> : ""}
			<select
				{...register(name, generateValidationRules(label, {required}))}
				{...rest}
				{...(className && {className})}
			>
				{options ?? ""}
			</select>
		</div>
	)
}

export default Select;