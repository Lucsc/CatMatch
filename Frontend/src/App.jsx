import { BrowserRouter, Link, Routes, Route } from 'react-router-dom'
import Leaderboard from './Leaderboard/Leaderboard'
import Vote from './Vote/Vote'
import Monitor from './Monitor/Monitor'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Leaderboard />} />
        <Route path="/vote" element={<Vote />} />
        <Route path="/monitor" element={<Monitor />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
