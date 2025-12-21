import express from "express"
import morgan from "morgan"
import "functional-extensions"
import { router } from "./routes"

console.log("Server fährt hoch...")

const PORT = process.env.PORT || 9865

const app = express()
app.use( morgan(":method :url :status :res[content-length] - :response-time ms"))
app.use(router)

const server = app.listen(PORT, () => console.log(`Listening on ${PORT}`))

process.on('SIGTERM', shutdown)
process.on('SIGINT', shutdown)

function shutdown() {
    console.log('SIGTERM signal received: closing HTTP server')
    server.close(() => console.log('Server herunter gefahren'))
}


// async function serveAudioTrack(req: express.Request, res: express.Response, track: string) {
//     await serveRange(req, res, track, "audio/mp3")
// }

// async function serveAlbums(res: express.Response, directory: string) {
//     const files = await fs.readdir(directory)  
//     res.send(JSON.stringify({ files }))
// }

// async function serveMusic(req: express.Request, res: express.Response) {
//     try {
//         const url = req.url.substr(RELATIVE_URL.length + 7)
//         const file = `${MUSIC_PATH}/${decodeURI(url)}`.replace(/\+/gi, " ")
//         if (file.toLowerCase().endsWith(".mp3")) 
//             await serveAudioTrack(req, res, file)
//         else
//             await serveAlbums(res, file)
//     }
//     catch (err) {
//         // Handle file not found
//         if (err !== null && err.code === 'ENOENT') {
//             res.sendStatus(404);
//         }
//     }
// }

// router.route(`${RELATIVE_URL}/music`).get(serveMusic)
// router.route(`${RELATIVE_URL}/music/*`).get(serveMusic)
