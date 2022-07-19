# Standard and error output (no difference for now).

# standard output
print('standard')

# error output
import sys
sys.stderr.write('error1\n')
sys.stderr.write('error2') #! last line is not printed withot \n
