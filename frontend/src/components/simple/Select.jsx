const Select = ({className, id, data, label, value, name, onChange}) => {
	const options = data?.map((item, index) => {
		return (
			<option value={item.value} key={index}>
				{item.label}
			</option>
		);
	})
	return (
		<div className={"selector"}>
			{label ? (<label>{label}</label>) : ''}
			<select
				value={value ?? ""}
				name={name}
				{...(className && {className})}
				id={id}
				onChange={(e) => onChange(name, e.target.value)}
			>
				{options ?? ""}
			</select>
		</div>
	)
}

export default Select;