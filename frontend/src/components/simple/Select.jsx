const Select = ({className, id, data, value, name, onChange}) => {
	const options = data.map((item, index) => {
		return (
			<option value={item.value} key={index}>
				{item.label}
			</option>
		);
	})
	return (
		<select
			value={value ?? ""}
			name={name}
			className={className}
			id={id}
			onChange={(e) => onChange(name, e.target.value)}>
			{options ?? ""}
		</select>
	)
}

export default Select;