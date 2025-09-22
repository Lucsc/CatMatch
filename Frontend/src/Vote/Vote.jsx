import { useEffect, useState } from "react";
import './Vote.css';
import logo from '../assets/logo.png';
import LoadingPage from "../utils/LoadingPage";
import ErrorPage from "../utils/ErrorPage";

const API_URL = import.meta.env.VITE_API_URL || '/api';

export default function Vote() {
    const [pair, setPair] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [matchesCount, setMatchesCount] = useState(0);

    useEffect(() => {
        fetchPair();
        fetchMatchesCount();
    }, []);

    const fetchPair = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await fetch(`${API_URL}/cats/random`);
            if (!response.ok) throw new Error("Error while fetching random pair");
            const data = await response.json();
            setPair(data);
        } catch(err) {
            setError("Il y a eu une erreur lors de la récupération des images.");
        } finally {
            setLoading(false);
        }
    }

    const fetchMatchesCount = async () => {
        try {
            const res = await fetch(`${API_URL}/cats/matchesCount`);
            if (!res.ok) return;
            const data = await res.json();
            setMatchesCount(data.count);
        } catch (e) {
        }
    }

    const vote = async(catId) => {
        try {
            const response = await fetch(`${API_URL}/cats/vote/${catId}`, { method: 'POST' });
            if (!response.ok) throw new Error("Error while voting");
            await fetchPair();
            await fetchMatchesCount();
        } catch(err) {
            setError("Il y a eu une erreur lors de l'enregistrement du vote.");
        }
    }

    let content;

    if (loading) content = <div><LoadingPage/></div>;
    else if (error) content = <ErrorPage message={error} />;
    else if (pair.length === 0) content = <ErrorPage message="Aucune paire de chats disponible." />;
    else 
        content = <div className="cat-pair">
                    {pair.map((cat, index) => (
                        <div key={cat.id} className="cat-card">
                            <img src={cat.url} alt={`Chat ${cat.id}`} className="cat-image"/>
                            <div className="cat-info">
                                <div className="cat-label">Chat Mignon {index + 1}</div>
                                <div className="cat-score">Score : {cat.score} pts</div>
                                <button className="like-button" onClick={(e) => { e.stopPropagation(); vote(cat.id); }}>
                                    J'aime
                                </button>
                            </div>
                        </div>
                    ))}
                </div>


    return (
        <div className="vote-page">
            <header className="site-header">
                <div className="logo">
                    <img src={logo} alt="Cat Icon" style={{ width: '40px', height: '40px' }} />
                    CATMASH
                </div>
            </header>

            <main className="vote-main">
                {content}
            </main>

            <footer className="site-footer">
                <div className="footer-inner">
                    <button className="footer-button" onClick={() => window.location.href = '/'}>Voir le classement des chats</button>
                    <div className="matches-count">{matchesCount} matchs joués</div>
                </div>
            </footer>
        </div>
    )
}