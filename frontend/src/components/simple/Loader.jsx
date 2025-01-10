import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import classNames from "classnames";

const Loader = ({type}) => {
	return (
		<div className={classNames("loader", type === "container" ? "loader-container" : null)}>
			<Box>
				<CircularProgress size={"10rem"} thickness={3}/>
			</Box>
		</div>
	)
}

export default Loader;