import errorCat from '../assets/ErrorCat.png';
import './Utils.css';

export default function ErrorPage({ message }) {
    return (
        <div className="error-page">
            <img src={errorCat} alt="Sad Cat" className="error-cat" />
            <p>{message}</p>
            <p>Veuillez réessayer plus tard.</p>
        </div>
    )
}