const Input = ({min, max, type, name, id, onChange, step, defaultValue, className, value}) => {
	return (
		<input
			className={`${className} custom-input-class`}
			type={type}
			min={min}
			max={max}
			name={name}
			id={id}
			step={step}
			defaultValue={defaultValue}
			onChange={(e) => onChange(name, e.target.value)} //Propagate the new value to the form
		/>

	)
}

export default Input;