import React, {useEffect, useState} from 'react';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export default function Leaderboard() {
    const [cats, setCats] = useState([]);

    useEffect(() => {
        fetch(`${API_URL}/cats`)
            .then(response => response.json())
            .then(data => setCats(data))
            .catch(error => console.error('Error fetching cats:', error));
    }, []);

    return (
        <div>
            <h1>Cat Leaderboard</h1>
            <table>
                <thead>
                <tr>
                    <th>Rank</th>
                    <th>Image</th>
                    <th>Score</th>
                </tr>
                </thead>
                <tbody>
                {cats.sort((a, b) => b.score - a.score).map((cat, index) => (
                    <tr key={cat.id}>
                        <td>{index + 1}</td>
                        <td><img src={cat.url} alt={`Cat ${cat.id}`} width="100"/></td>
                        <td>{cat.score}</td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    )
}