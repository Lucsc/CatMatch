import { BrowserRouter, Link, Routes, Route } from 'react-router-dom'
import Leaderboard from './Leaderboard/Leaderboard'
import Vote from './Vote/Vote'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Leaderboard />} />
        <Route path="/vote" element={<Vote />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
