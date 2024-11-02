const Input = ({min, max, children, type, name, id, label, onChange, step, defaultValue, className, value}) => {
	return (
		<div className={"input-field"}>
			<label className={"input-field-label"} for={name}>
				{label}
			</label>
			{children}
			<input
				{...(className && {className})}//conditional rendering of the class
				type={type}
				min={min}
				max={max}
				name={name}
				id={id}
				step={step}
				defaultValue={defaultValue}
				onChange={(e) => onChange(name, e.target.value)} //Propagate the new value to the form
			/>
		</div>

	)
}

export default Input;