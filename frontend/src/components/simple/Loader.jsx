import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

const Loader = () => {
	return (
		<div className={"loader"}>
			<Box>
				<CircularProgress size={"10rem"} thickness={3}/>
			</Box>
		</div>
	)
}

export default Loader;