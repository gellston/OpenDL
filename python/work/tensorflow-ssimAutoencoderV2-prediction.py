import numpy as np
import cv2 as cv2

import tensorflow as tf
import os
import time

from util.helper import extract_box_from_featuremaps_with_score
from util.helper import filter_overlap_boxes



sess = tf.Session()
saver = tf.train.import_meta_graph('C:/Github/OpenDL/python/pretrained-model/model_anomalydetection_SSIMAUTOV2/model_anomalydetection_SSIMAUTOV2.meta')
saver.restore(sess, tf.train.latest_checkpoint('C:/Github/OpenDL/python/pretrained-model/model_anomalydetection_SSIMAUTOV2/'))

graph = tf.get_default_graph()
phase = graph.get_tensor_by_name("model_anomalydetection_SSIMAUTOV2/phase:0")
X = graph.get_tensor_by_name("model_anomalydetection_SSIMAUTOV2/RealTarget:0")
Heatmap = graph.get_tensor_by_name("model_anomalydetection_SSIMAUTOV2/Sigmoid:0")



### FPS CODE
start_time = time.time()
x = 1 # displays the frame rate every 1 second
counter = 0
### FPS CODE

cv2.namedWindow('image', cv2.WINDOW_NORMAL)
cv2.resizeWindow('image', 256, 256)

input_images = []


# crop = img[100:480 - 100, 200:640 - 200]
img = cv2.imread("C:/Downloads/wood.tar/wood/wood/test/scratch/007.png", cv2.IMREAD_GRAYSCALE)
resize = cv2.resize(img, dsize=(256, 256), interpolation=cv2.INTER_AREA)
resize = resize[50:178, 50:178]
resize = np.array(resize)
reshape = resize.reshape([128, 128, 1])
npImage = reshape / 255
input_images.append(npImage)


output = sess.run(Heatmap, feed_dict={X: input_images, phase: False})
reconstuction = output[0]*255
diff_image = np.absolute(reconstuction - reshape)

max_value = np.amax(diff_image)
min_value = np.amin(diff_image)
mid_value = (max_value - min_value)
print('max_value !! = ', max_value)
print('min_value !! = ', min_value)





threshold_map = (diff_image > 25.0) * 255
dilated_map = cv2.medianBlur(threshold_map.astype(np.uint8), 3)





cv2.imshow('image', reconstuction.astype(np.uint8))
cv2.imshow('diff_image', diff_image.astype(np.uint8))
cv2.imshow('threshold_map', threshold_map.astype(np.uint8))
cv2.imshow('dilated_map', dilated_map.astype(np.uint8))
#cv2.imshow('adaptive_threshold', otsu_threshold.astype(np.uint8))

cv2.waitKey()