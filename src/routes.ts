import { Router } from "express"
import serveStatic from "serve-static"
import { serveFile } from "./fireplayer.js"
import { getFiles, putFile } from "./commander-engine.js"

export const router = Router()

const VIDEO_PATH = process.env.VIDEO_PATH || '/video'
const MUSIC_PATH = process.env.MUSIC_PATH || '/music'
const PICTURE_PATH = process.env.PICTURE_PATH || '/video'
const APK_PATH = process.env.APK_PATH || '/apk'
console.log("VIDEO_PATH", VIDEO_PATH)
console.log("MUSIC_PATH", MUSIC_PATH)
console.log("PICTURE_PATH", PICTURE_PATH)
console.log("APK_PATH", APK_PATH)

router.get('/video{/*splat}', (req, res, n) => serveFile(VIDEO_PATH, req, res, n))
router.use('/video', serveStatic(VIDEO_PATH))

router.get('/pics{/*splat}', (req, res, n) => serveFile(PICTURE_PATH, req, res, n))
router.use('/pics', serveStatic(PICTURE_PATH))

router.get('/music{/*splat}', (req, res, n) => serveFile(MUSIC_PATH, req, res, n))
router.use('/music', serveStatic(MUSIC_PATH))

router.use('/apk', serveStatic(APK_PATH))

router.get('/getFiles{/*splat}', getFiles)
router.put('/putFile{/*splat}', putFile)

