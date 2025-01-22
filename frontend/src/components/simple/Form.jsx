import {useEffect, useState} from "react";
import classNames from 'classnames';
import {useForm, FormProvider} from 'react-hook-form';
import {memo} from "react";
import Slider from "./Slider.jsx";
import Select from "./Select.jsx";
import Input from "./Input.jsx";
import Button from "./Button.jsx";
import CountdownTimer from "./Countdown.jsx";
import {sanitizeData} from "../../services/helpers.jsx";

const Field = memo(({item, fieldClass, wClassName}) => {
  const commonProps = {
    ...item,
    className: fieldClass,
    wClassName,
  };

  let Component;
  switch (item.type) {
    case 'range':
      Component = Slider;
      break;
    case 'select':
      Component = Select;
      break;
    case 'title':
      return <h3 key={item.text} {...(fieldClass && {className: fieldClass})}>{item.text}</h3>;
    default:
      Component = Input;
      break;
  }

  return <Component key={item.name} {...commonProps} />;
});

const Form = ({data, onSubmit, className, showAsterisks = true, isDisabled, disabledFormHint, countdownStart = 0}) => {
  const [secondsToGo, setSecondsToGo] = useState(countdownStart);

  const methods = useForm({
    defaultValues: {
      ...data.fields.reduce((acc, item) => {
        if (item.type !== 'title') {
          acc[item.name] = item.value ?? "";
        }
        return acc;
      }, {}),
    },
  });

  useEffect(() => {
	  if (secondsToGo) {
        const timer = setTimeout(() => {
          setSecondsToGo(0)
        }, secondsToGo * 1000);

        return () => clearTimeout(timer);
	  }
  }, [secondsToGo])

  const flushForm = () => {
    methods.reset();
  };

  const customHandleSubmit = async (formData) => {
    setSecondsToGo(await onSubmit(sanitizeData(formData), flushForm) ?? 0);
  };

  const {text: buttonText, className: btnClassName, ...buttonRest} = data.button ?? {};
  return (
    <FormProvider {...methods}>
      <form noValidate className={classNames(className, isDisabled ? "form-disabled" : null)}
            onSubmit={methods.handleSubmit(customHandleSubmit)}>
        <fieldset disabled={isDisabled || secondsToGo}>
          {data.fields.sort((a, b) => a.pos - b.pos).map(({pos, value, ...item}, index) => {
            item.showAsterisks = showAsterisks;
            return <Field
              key={index}
              item={item}
              fieldClass={classNames(data.fieldClass, item.className)}
              wClassName={classNames(data.wClassName, item.wClassName)}
            />;
          })}
          {secondsToGo ? (
            <CountdownTimer initialTime={secondsToGo}/>
          ) : (
            buttonText && (
              <Button
                className={classNames(btnClassName, isDisabled ? "btn-disabled" : null)} {...buttonRest}
                onSubmit={(e) => {
                  return isDisabled ? e.preventDefault() : methods.handleSubmit(customHandleSubmit);
                }}
                disabled={isDisabled}
              >
                {buttonText}
              </Button>
            )
          )}
        </fieldset>
        {isDisabled && (
          <div className={"form-disabler"}>
            <span className={"form-disabler-hint"}>{disabledFormHint}</span>
          </div>
        )}
      </form>
    </FormProvider>
  );
};

export default Form;
