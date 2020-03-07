import tensorflow as tf
import matplotlib.pyplot as plt
import cv2 as cv2
import numpy as np

from model.model_anomalydetection_anogan import model_anomalydetection_anogan
from util.anogan_dataloader import anogan_dataloader

train_loader = anogan_dataloader('C://Github//OpenDL//python//dataset//anoGan_Test//')
validation_loader = anogan_dataloader('C://Github//OpenDL//python//dataset//anoGan_Test//')

train_epoch = 10000
batch_size = 6
sample_size = train_loader.size()
total_batch = int(sample_size / batch_size)
target_accuracy = 0.95
learning_rate = 0.003

sess = tf.Session()
model = model_anomalydetection_anogan(sess=sess, name="model_anomalydetection_anogan")

global_variable_initializer = tf.compat.v1.global_variables_initializer()
print('global variable initializer name : ', global_variable_initializer.name)
sess.run(global_variable_initializer)

## save model file
#saver = tf.compat.v1.train.Saver()
#saver.save(sess, 'C:/Github/OpenDL/python/pretrained-model/model_segmentation_HySegnetV1/model_segmentation_HySegnetV1')
#tf.train.write_graph(sess.graph_def, "", 'C:/Github/OpenDL/python/pretrained-model/model_segmentation_HySegnetV1/model_segmentation_HySegnetV1.pbtxt', as_text=True)


discriminator_loss_graph = []
generator_loss_graph = []
accuracy_graph = []
print('Learning start.')

for step in range(train_epoch):

    train_loader.clear()
    discriminator_total_loss = 0
    generator_total_loss = 0
    avg_accuracy = 0
    accuracy = 0
    for batch in range(total_batch):
        #train_loader.clear()
        input_images = train_loader.load(batch=batch_size)

        if input_images is None:
            train_loader.clear()
            break

        _, discriminator_loss = model.train_discriminator(input_images, keep_prop=True, learning_rate=learning_rate)
        _, generator_loss = model.train_generator(input_images, keep_prop=True, learning_rate=learning_rate)

        discriminator_total_loss += discriminator_loss / total_batch
        generator_total_loss += generator_loss / total_batch

    validation_loader.clear()
    for validation_step in range(100):
        validation_images = validation_loader.load(batch=1)
        accuracy = model.get_accuracy(validation_images, keep_prop=False)
        avg_accuracy += (accuracy / 100)
        print('Validation Step: ', '%04d' % (validation_step + 1), ' step accuracy = ', '{:.9f}'.format(accuracy),)

    output_images = model.reconstruct(validation_images, keep_prop=False)
    output_reshape = (output_images[0] + 1)* 127.5
    input_image = (validation_images[0] + 1)* 127.5



    cv2.imshow('reconstruced image', output_reshape.astype(np.uint8))
    cv2.imshow('input image', input_image.astype(np.uint8))
    cv2.waitKey(10)
    validation_loader.clear()

    accuracy_graph.append(accuracy)
    discriminator_loss_graph.append(discriminator_total_loss)
    generator_loss_graph.append(generator_total_loss)

    print('Epoch : ', '%04d' % (step + 1),
          'discriminator_loss =', '{:.9f}'.format(discriminator_total_loss),
          'generator_loss =', '{:.9f}'.format(generator_total_loss),
          'accuracy =', '{:.9f}'.format(avg_accuracy))
    ##if avg_accuracy > target_accuracy:
    ##   break;

#tf.train.write_graph(sess.graph.as_graph_def(),"./pretrained-models/model_segmentation_HySegnetV1/", "model_segmentation_HySegnetV1.pb")
#saver = tf.train.Saver(tf.global_variables())
#saver.save(sess, './pretrained-models/model_segmentation_HySegnetV1/model_segmentation_HySegnetV1.ckpt')

plt.plot(discriminator_loss_graph)
plt.plot(generator_loss_graph)
plt.plot(accuracy_graph)
plt.ylabel('discriminator_loss, generator_loss,  accuracy')
plt.legend(['discriminator_loss', 'generator_loss', 'accuracy'], loc='upper left')
#plt.savefig('./pretrained-models/model_segmentation_HySegnetV1/model_segmentation_HySegnetV1.png')
plt.show()

print('Learning finished.')