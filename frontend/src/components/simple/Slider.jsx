import Input from "./Input.jsx";

const Slider = ({minText, maxText, isSplit, ...rest}) => {
	return (
		<Input {...rest}>
			<div className={'input-field-slider-range'}>
				<span className={"slider-min"}>{minText ?? rest.min}</span>
				<span className={"slider-current"}>
					{isSplit ? `${rest.max - rest.value} / ${rest.value}` : rest.value }
				</span>
				<span className={"slider-max"}>{maxText ?? rest.max}</span>
			</div>
		</Input>
	)
}

export default Slider;