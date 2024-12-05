import Input from "./Input.jsx";
import {useFormContext} from "react-hook-form";

const Slider = ({minText, maxText, isSplit, ...rest}) => {
	const {watch} = useFormContext()
	const value = watch(rest.name)

	return (
		<Input {...rest}>
			<div className={'input-field-slider-range'}>
				<span className={"slider-min"}>{minText ?? rest.min}</span>
				<span className={"slider-current"}>
					{isSplit ? `${rest.max - value} / ${value}` : value}
				</span>
				<span className={"slider-max"}>{maxText ?? rest.max}</span>
			</div>
		</Input>
	)
}

export default Slider;