// Without EnableTaskPromiseConversion this script works incorrectly:
// instead of sleeping for 5 seconds it shows the result immediately.

function sleep(milliseconds) {
    return clr.System.Threading.Tasks.Task.Delay(milliseconds)
}

function job(proc) {
    return far.PostJobAsync(host.proc(0, proc))
}

async function test(milliseconds) {
    await sleep(milliseconds)
    await job(() => far.Message(`done in ${milliseconds} milliseconds`))
}

test(parseInt(args.milliseconds ?? 5000))
