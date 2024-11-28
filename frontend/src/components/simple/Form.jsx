import classNames from 'classnames';
import {useForm, FormProvider} from 'react-hook-form';
import {memo} from "react";
import Slider from "./Slider.jsx";
import Select from "./Select.jsx";
import Input from "./Input.jsx";
import Button from "./Button.jsx";
import {sanitizeData} from "../../services/helpers.jsx";

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

	const flushForm = () => {
		methods.reset()
	}

	const customHandleSubmit = (data) => {
		//We want to not allow different <script>alert()</script> and something else propagated. That is why we take care of special symbols here
		onSubmit(sanitizeData(data), flushForm);
	}

	const {text: buttonText, ...buttonRest} = data.button ?? {};
	return (
		<FormProvider {...methods}>
			<form noValidate={true} onSubmit={methods.handleSubmit(customHandleSubmit)}>
				{
					data.fields.sort(function (a, b) {
						return a.pos - b.pos;
					}).map(({pos, value, ...item}) => {
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