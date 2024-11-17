import {useState, useCallback, memo} from "react";
import Slider from "./Slider.jsx";
import Select from "./Select.jsx";
import Input from "./Input.jsx";
import Button from "./Button.jsx";

const Field = memo(({item, value, onChange, fieldClass, wClassName}) => {
	const commonProps = {
		...item,
		value,
		className: fieldClass,
		wClassName,
		onChange
	}

	let Component;
	switch (item.type) {
		case 'range':
			Component = Slider;
			break;
		case 'select':
			Component = Select;
			break;
		default:
			Component = Input;
			break;
	}

	return <Component key={item.name} {...commonProps} />
});

const Form = ({data, onSubmit}) => {
	const [values, setValues] = useState({
		...data.fields.reduce((acc, item) => {
			acc[item.name] = item.default ?? null;
			return acc;
		}, {}),
	});

	const onChange = useCallback((name, value) => {
		setValues((prevValues) => (
			{...prevValues, [name]: value}
		));
	}, []);

	const flushFields = () => {
		setValues({})
	}

	const handleSubmit = (e) => {
		e.preventDefault();
		onSubmit(values, flushFields);
	}
	const {text: buttonText, ...buttonRest} = data.button;

	return (
		<form onSubmit={handleSubmit}>
			{
				data.fields.map((item) => (
					<Field
						key={item.name}
						item={item}
						value={values[item.name]}
						onChange={onChange}
						fieldClass={data.fieldClass}
						wClassName={data.wClassName}
					/>
				))
			}
			<Button {...buttonRest} onSubmit={handleSubmit}>
				{buttonText}
			</Button>
		</form>
	)
}

export default Form;