import DOMPurify from "dompurify";
import {useForm, FormProvider} from 'react-hook-form';
import {memo} from "react";
import Slider from "./Slider.jsx";
import Select from "./Select.jsx";
import Input from "./Input.jsx";
import Button from "./Button.jsx";

const Field = memo(({item, fieldClass, wClassName}) => {
	const commonProps = {
		...item,
		className: fieldClass,
		wClassName,
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
	const methods = useForm();

	const customHandleSubmit = (data) => {
		//We want to not allow different <script>alert()</script> and something else propagated. That is why we take care of special symbols here
		const sanitizedData = Object.fromEntries(
			Object.entries(data).map(([k, v]) => {
				if (typeof v === "string") {
					return [k, DOMPurify.sanitize(v)]
				}
				return [k, v];
			})
		)
		onSubmit(sanitizedData);
	}
	const {text: buttonText, ...buttonRest} = data.button;

	return (
		<FormProvider {...methods}>
			<form noValidate={true} onSubmit={methods.handleSubmit(customHandleSubmit)}>
				{
					data.fields.map(({pos, ...item}) => {
						return <Field
							key={item.name}
							item={item}
							fieldClass={data.fieldClass}
							wClassName={data.wClassName}
						/>
					})
				}
				<Button {...buttonRest} onSubmit={methods.handleSubmit(customHandleSubmit)}>
					{buttonText}
				</Button>
			</form>
		</FormProvider>
	)
}

export default Form;