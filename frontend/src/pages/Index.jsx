import RequestBuilder from '../components/simple/RequestBuilder.jsx';
import MapSection from '../components/simple/MapSection';
import Footer from '../components/Footer.jsx';
import Header from "../components/Header.jsx";

const Index = () => {
	return (
		<div className="container">
			<Header/>
			<RequestBuilder/>
			<MapSection/>
			<Footer/>
		</div>
	)
}

export default Index;