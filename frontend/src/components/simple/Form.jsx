import classNames from 'classnames';
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
		case 'title':
			return <h3 {...(fieldClass && {className: fieldClass})}>{item.text}</h3>;
		default:
			Component = Input;
			break;
	}

	return <Component key={item.name} {...commonProps} />
});

const Form = ({data, onSubmit}) => {
	const methods = useForm({
		defaultValues: {
			...data.fields.reduce((acc, item) => {
				if (item.type !== 'title') {
					acc[item.name] = item.value ?? "";
				}
				return acc;
			}, {})
		}
	});

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

	const {text: buttonText, ...buttonRest} = data.button ?? {};
	return (
		<FormProvider {...methods}>
			<form noValidate={true} onSubmit={methods.handleSubmit(customHandleSubmit)}>
				{
					data.fields.map(({pos, value, ...item}) => {
						return <Field
							key={item.name}
							item={item}
							fieldClass={classNames(data.fieldClass, item.className)}
							wClassName={classNames(data.wClassName, item.wClassName)}
						/>
					})
				}
				{buttonText ?
					<Button {...buttonRest} onSubmit={methods.handleSubmit(customHandleSubmit)}>
						{buttonText}
					</Button> : ''}
			</form>
		</FormProvider>
	)
}

export default Form;