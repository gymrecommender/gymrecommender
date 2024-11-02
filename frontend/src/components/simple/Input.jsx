const Input = ({children, onChange, label, className, value, ...rest}) => {
	return (
		<div className={"input-field"}>
			<label className={"input-field-label"} htmlFor={rest.name}>
				{label}
			</label>
			{children}
			<input
				{...rest}
				{...(className && {className})}//conditional rendering of the class
				value={value ?? ''}
				onChange={(e) => onChange(rest.name, e.target.value)} //Propagate the new value to the form
			/>
		</div>

	)
}

export default Input;