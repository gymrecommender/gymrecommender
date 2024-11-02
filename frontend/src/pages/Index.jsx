import Sliders from '../components/simple/Sliders';
import LocationControls from '../components/simple/LocationControls';
import MapSection from '../components/simple/MapSection';
import Footer from '../components/Footer.jsx';
import Header from "../components/Header.jsx";

const Index = () => {
	return (
		<div className="container">
			<Header/>
			<Sliders/>
			<LocationControls/>
			<MapSection/>
			<Footer/>
		</div>
	)
}

export default Index;