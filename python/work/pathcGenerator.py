import tensorflow as tf
import matplotlib.pyplot as plt
import cv2 as cv2
import numpy as np


from util.randomPatchGenerator import randomPatchGenerator

patchGenerator = randomPatchGenerator('C://Github//OpenDL//python//dataset//wood//',
                                    'C://Github//OpenDL//python//dataset//woodPatch//')



patchGenerator.generator(color=1, width=128, height=128, shape=[128, 128, 3], resizeWidth=256, resizeHeight=256)