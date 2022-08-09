
function show(context) {
    console.log(args.runner.Info(context ?? 0))
}

function s() {
    args.runner.StepInto()
}

function v() {
    args.runner.StepOver()
}

function o() {
    args.runner.StepOut()
}

function c() {
    args.runner.Continue()
}

function q() {
    args.runner.Stop()
}
