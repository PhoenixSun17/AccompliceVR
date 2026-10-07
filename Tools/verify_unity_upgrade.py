"""Run migration checks only in the upgrade project or its disposable copies."""
import argparse
from datetime import datetime, timezone
from pathlib import Path
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('operation', choices=['build', 'validate', 'native'])
    parser.add_argument('--project', choices=['upgrade', 'baseline', 'clean'], default='upgrade')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    if root != Path(r'C:\AccompliceVR - 副本').resolve():
        parser.error('This migration runner is restricted to the authorized upgrade copy.')
    project = root
    version = '6000.6.4f1'
    if args.project == 'baseline':
        project = root / 'Builds' / 'Unity2022BaselineProject'
        version = '2022.3.16f1'
    elif args.project == 'clean':
        project = root / 'Builds' / 'Unity66CleanImport'
    protected = Path(r'C:\AccompliceVR').resolve()
    if project.resolve() == protected or protected in project.resolve().parents:
        parser.error('Refusing to launch the protected original.')
    if not project.resolve().is_relative_to(root):
        parser.error('Project must remain inside the upgrade copy.')
    for path in [project, *project.parents]:
        if path.is_symlink() or path.is_junction():
            parser.error('Junction/symlink project paths are not allowed.')
    version_file = project / 'ProjectSettings' / 'ProjectVersion.txt'
    if not version_file.exists() or f'm_EditorVersion: {version}' not in version_file.read_text():
        parser.error(f'Project must already exist and target {version}.')
    editor = Path(r'C:\Program Files\Unity\Hub\Editor') / version / 'Editor' / 'Unity.exe'
    log_root = root / 'Logs' / 'Unity66Upgrade'
    log_root.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    log = log_root / f'{args.project}-{args.operation}-{stamp}.log'
    method = {'build': 'BuildAll', 'validate': 'ValidateScenes', 'native': 'NativeSmoke'}[args.operation]
    command = [str(editor), '-batchmode', '-quit', '-projectPath', str(project),
               '-buildTarget', 'Win64', '-executeMethod', f'AccompliceVRUpgradeBuild.{method}',
               '-accompliceAllowBaselineMissingPortal', '-logFile', str(log)]
    command += ['-force-d3d11'] if args.operation == 'native' else ['-nographics']
    if args.project == 'baseline':
        command += ['-accompliceBuildRoot', 'Builds/Unity2022']
    print(f'Unity {version}: {args.operation}, {args.project}', flush=True)
    print(f'Log: {log}', flush=True)
    startup = subprocess.STARTUPINFO()
    startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow = 0
    result = subprocess.run(command, cwd=project, startupinfo=startup)
    print(f'Unity exit code: {result.returncode}', flush=True)
    return result.returncode


if __name__ == '__main__':
    raise SystemExit(main())
